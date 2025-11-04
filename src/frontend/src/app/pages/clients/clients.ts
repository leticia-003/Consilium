import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client';
import { disableDebugTools } from '@angular/platform-browser';
import { debounceTime } from 'rxjs/internal/operators/debounceTime';
import { Subject } from 'rxjs';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-clients',
  standalone: true,
  template: `
    <section class="page">
      <div class="page-header">
        <app-page-title title="Clients"></app-page-title>
        <div class="clients-toolbar">
          <div class="search-box" *ngIf="!loading && !errorMessage && filteredClients.length > 0">
            <i class="fa fa-search search-icon" aria-hidden="true"></i>
            <input class="search" type="search" placeholder="Search" (input)="onSearch($event.target.value)" />
          </div>

          <app-button label="Add New Client" icon="fa-plus" [link]="['/create-client']" variant="primary"></app-button>
        </div>
      </div>

      <div class="clients-filters" *ngIf="!loading && !errorMessage && filteredClients.length > 0">
        <button class="filter-pill" [class.active]="selectedFilter === 'all'" (click)="applyFilter('all')">
          <span class="filter-label">All Clients</span>
          <span class="filter-count">{{ totalClients }}</span>
        </button>

        <button class="filter-pill" [class.active]="selectedFilter === 'active'" (click)="applyFilter('active')">
          <span class="filter-label">Active Clients</span>
          <span class="filter-count">{{ activeClients }}</span>
        </button>

        <button class="filter-pill" [class.active]="selectedFilter === 'inactive'" (click)="applyFilter('inactive')">
          <span class="filter-label">Inactive Clients</span>
          <span class="filter-count">{{ inactiveClients }}</span>
        </button>
      </div>

      <div *ngIf="loading" class="loading">Loading clients...</div>
      <div *ngIf="errorMessage" class="error">{{ errorMessage }}</div>

      <div *ngIf="!loading && !errorMessage">
        <table class="clients-table" *ngIf="filteredClients.length > 0">
          <thead>
            <tr>
              <th class="col-account">
                <button class="th-sort" (click)="toggleSort('name')" [attr.aria-sort]="ariaSort('name')">
                  <span>Account</span>
                  <span class="chev-wrap" aria-hidden="true">
                    <i class="chev chev-default fas fa-sort"></i>
                    <i class="chev chev-asc fas fa-sort-up"></i>
                    <i class="chev chev-desc fas fa-sort-down"></i>
                  </span>
                </button>
              </th>
              <th class="col-email">Email</th>
              <th class="col-nif">
                <button class="th-sort" (click)="toggleSort('nif')" [attr.aria-sort]="ariaSort('nif')">
                  <span>NIF</span>
                  <span class="chev-wrap" aria-hidden="true">
                    <i class="chev chev-default fas fa-sort"></i>
                    <i class="chev chev-asc fas fa-sort-up"></i>
                    <i class="chev chev-desc fas fa-sort-down"></i>
                  </span>
                </button>
              </th>
              <th class="col-date">
                <button class="th-sort" (click)="toggleSort('createdAt')" [attr.aria-sort]="ariaSort('createdAt')">
                  <span>Date</span>
                  <span class="chev-wrap" aria-hidden="true">
                    <i class="chev chev-default fas fa-sort"></i>
                    <i class="chev chev-asc fas fa-sort-up"></i>
                    <i class="chev chev-desc fas fa-sort-down"></i>
                  </span>
                </button>
              </th>
              <th class="col-status">Status</th>
              <th class="col-actions"></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let c of filteredClients; let i = index">
              <td class="col-account">
                <div class="account-name">{{ c.name }}</div>
              </td>
              <td class="col-email">{{ c.email || '—' }}</td>
              <td class="col-nif">{{ c.nif || '—' }}</td>
              <td class="col-date">{{ c.createdAt ? (c.createdAt | date:'dd MMM, yyyy') : '—' }}</td>
              <td class="col-status">
                <span class="badge" [ngClass]="{ 'badge-active': c.isActive, 'badge-inactive': !c.isActive }">
                  {{ c.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="col-actions">
                <button class="btn-ellipsis" aria-label="Actions">⋯</button>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="pagination">
          <button (click)="goToPage(1)" [disabled]="currentPage === 1">First</button>
          <button (click)="goToPage(currentPage - 1)" [disabled]="currentPage === 1">Previous</button>

          <span>Page {{ currentPage }} of {{ totalPages }}</span>

          <span>
            Go to:
            <input
              type="number"
              min="1"
              [max]="totalPages"
              [(ngModel)]="goToPageNumber"
              style="width: 60px; text-align: center;"
            />
            <button (click)="goToPage(goToPageNumber)">Go</button>
          </span>

          <button (click)="goToPage(currentPage + 1)" [disabled]="currentPage === totalPages">Next</button>
          <button (click)="goToPage(totalPages)" [disabled]="currentPage === totalPages">Last</button>
        </div>


        <div *ngIf="filteredClients.length === 0" class="empty">No clients to display.</div>
      </div>
    </section>
  `,
  styleUrls: ['./clients.css'],
  imports: [PageTitleComponent, CommonModule, ButtonComponent, FormsModule],
})

export class ClientsComponent implements OnInit, OnDestroy {
  clients: Client[] = [];
  filteredClients: Client[] = [];
  loading = false;
  errorMessage = '';
  selectedFilter: 'all' | 'active' | 'inactive' = 'all';
  searchTerm = '';
  sortBy: 'name' | 'nif' | 'createdAt' | null = null;
  sortDir: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  totalPages = 1;
  pageSize = 10;
  totalCount = 0;
  goToPageNumber = 1;

  private searchSubject = new Subject<string>();
  private searchSubscription: any;

  constructor(private clientService: ClientService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject
      .pipe(debounceTime(500))
      .subscribe((term: string) => this.fetchClients(term));

    this.loadClients();
  }

  loadClients(page = 1): void {
    this.loading = true;
    this.errorMessage = '';
    this.currentPage = page;

    this.clientService.getClients({ page: this.currentPage, limit: this.pageSize }).subscribe({
      next: (data: any) => {
        const apiData = Array.isArray(data) ? data : data.data;
        this.clients = this.transformApiData(apiData);

        this.totalCount = data.meta?.totalCount || this.clients.length;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        this.filteredClients = [...this.clients];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load clients', err);
        this.errorMessage = 'Failed to load clients.';
        this.loading = false;
      }
    });
  }


  onSearch(term: string): void {
    this.searchTerm = (term || '').trim();
    this.searchSubject.next(this.searchTerm);
  }

  ariaSort(column: 'name' | 'nif' | 'createdAt') {
    if (this.sortBy !== column) return 'none';
    return this.sortDir === 'asc' ? 'ascending' : 'descending';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;

    if (this.searchTerm) {
      this.fetchClients(this.searchTerm, page);
    } else {
      this.loadClients(page);
    }
  }

  private fetchClients(term: string, page = 1): void {
    this.loading = true;
    this.currentPage = page;

    this.clientService.getClients({ search: term, page: this.currentPage, limit: this.pageSize }).subscribe({
      next: (data: any) => {
        const apiData = Array.isArray(data) ? data : data.data;
        this.clients = this.transformApiData(apiData);

        this.totalCount = data.meta?.totalCount || this.clients.length;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        this.filteredClients = [...this.clients];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  private transformApiData(apiData: any[]): Client[] {
    return (apiData || []).map((c: any) => ({
      id: c.id,
      name: c.name,
      email: c.email,
      address: c.address,
      nif: c.nif,
      phone: c.phone,
      isActive: c.status?.toUpperCase() === 'ACTIVE',
      createdAt: c.createdAt || null
    }));
  }

  applyFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.selectedFilter = filter;
    if (filter === 'active') {
      this.filteredClients = this.clients.filter(c => c.isActive);
    } else if (filter === 'inactive') {
      this.filteredClients = this.clients.filter(c => !c.isActive);
    } else {
      this.filteredClients = [...this.clients];
    }
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
  }

  // Sorting helpers (still client-side)
  toggleSort(column: 'name' | 'nif' | 'createdAt') {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }

    const dir = this.sortDir === 'asc' ? 1 : -1;
    this.filteredClients.sort((a: any, b: any) => {
      let res = 0;
      if (column === 'name') res = a.name.localeCompare(b.name);
      if (column === 'nif') res = Number(a.nif) - Number(b.nif);
      if (column === 'createdAt') res = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      return res * dir;
    });
  }

  get totalClients(): number { return this.clients.length; }
  get activeClients(): number { return this.clients.filter(c => !!c.isActive).length; }
  get inactiveClients(): number { return this.clients.filter(c => !c.isActive).length; }
}