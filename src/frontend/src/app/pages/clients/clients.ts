import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client';

@Component({
  selector: 'app-clients',
  standalone: true,
  template: `
    <section class="page">
      <div class="page-header">
        <app-page-title title="Clients"></app-page-title>
        <div class="clients-toolbar">
          <div class="search-box">
            <i class="fa fa-search search-icon" aria-hidden="true"></i>
            <input class="search" type="search" placeholder="Search" (input)="onSearch($event.target.value)" />
          </div>
        </div>
      </div>

      <div class="clients-filters">
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
                <button class="th-sort" (click)="toggleSort('taxId')" [attr.aria-sort]="ariaSort('taxId')">
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
              <td class="col-nif">{{ c.taxId || '—' }}</td>
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

        <div *ngIf="filteredClients.length === 0" class="empty">No clients to display.</div>
      </div>
    </section>
  `,
  styleUrls: ['./clients.css'],
  imports: [PageTitleComponent, CommonModule]
})
export class ClientsComponent implements OnInit {
  clients: Client[] = [];
  filteredClients: Client[] = [];
  loading = false;
  errorMessage = '';
  selectedFilter: 'all' | 'active' | 'inactive' = 'all';
  searchTerm = '';
  sortBy: 'name' | 'taxId' | 'createdAt' | null = null;
  sortDir: 'asc' | 'desc' = 'asc';

  constructor(private clientService: ClientService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadClients();
  }

  loadClients(): void {
    this.loading = true;
    this.errorMessage = '';
    this.clientService.getClients().subscribe({
      next: (data) => {
        this.clients = data;
        this.updateFilteredClients();
        this.loading = false;
        // Força a deteção de mudanças
        try { this.cdr.detectChanges(); } catch (e) { /* noop */ }
      },
      error: (err) => {
        console.error('Failed to load clients', err);
        this.errorMessage = 'Failed to load clients.';
        this.loading = false;
        try { this.cdr.detectChanges(); } catch (e) { /* noop */ }
      }
    });
  }

  onSearch(term: string): void {
    this.searchTerm = (term || '').trim();
    this.updateFilteredClients();
  }

  applyFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.selectedFilter = filter;
    this.updateFilteredClients();
  }

  get totalClients(): number { return this.clients.length; }
  get activeClients(): number { return this.clients.filter(c => !!c.isActive).length; }
  get inactiveClients(): number { return this.clients.filter(c => !c.isActive).length; }

  private updateFilteredClients(): void {
    const q = (this.searchTerm || '').trim().toLowerCase();
    let list = [...this.clients];
    if (this.selectedFilter === 'active') {
      list = list.filter(c => !!c.isActive);
    } else if (this.selectedFilter === 'inactive') {
      list = list.filter(c => !c.isActive);
    }
    if (q) {
      list = list.filter(c => {
        const name = (c.name || '').toLowerCase();
        const email = (c.email || '').toLowerCase();
        const tax = (c.taxId || '').toLowerCase();
        return name.includes(q) || email.includes(q) || tax.includes(q);
      });
    }
    
    if (this.sortBy) {
      const dir = this.sortDir === 'asc' ? 1 : -1;
      list.sort((a, b) => {
        let res = 0;
        if (this.sortBy === 'name') {
          const an = (a.name || '').toString();
          const bn = (b.name || '').toString();
          res = an.localeCompare(bn, undefined, { sensitivity: 'base' });
        } else if (this.sortBy === 'taxId') {
          const at = (a.taxId || '').toString();
          const bt = (b.taxId || '').toString();
          
          const anNum = parseFloat(at.replace(/[^0-9.-]/g, ''));
          const bnNum = parseFloat(bt.replace(/[^0-9.-]/g, ''));
          if (!isNaN(anNum) && !isNaN(bnNum)) {
            res = anNum - bnNum;
          } else {
            res = at.localeCompare(bt, undefined, { sensitivity: 'base' });
          }
        } else if (this.sortBy === 'createdAt') {
          const ad = a.createdAt ? new Date(a.createdAt) : new Date(0);
          const bd = b.createdAt ? new Date(b.createdAt) : new Date(0);
          res = ad.getTime() - bd.getTime();
        }
        return res * dir;
      });
    }
    this.filteredClients = list;
  }

  toggleSort(column: 'name' | 'taxId' | 'createdAt') {
    if (this.sortBy === column) {
      // cycle: asc -> desc -> none
      if (this.sortDir === 'asc') {
        this.sortDir = 'desc';
      } else if (this.sortDir === 'desc') {
        this.sortBy = null;
      }
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }
    this.updateFilteredClients();
  }

  isSorted(column: 'name' | 'taxId' | 'createdAt', dir: 'asc' | 'desc') {
    return this.sortBy === column && this.sortDir === dir;
  }

  ariaSort(column: 'name' | 'taxId' | 'createdAt') {
    if (this.sortBy !== column) return 'none';
    return this.sortDir === 'asc' ? 'ascending' : 'descending';
  }


}
