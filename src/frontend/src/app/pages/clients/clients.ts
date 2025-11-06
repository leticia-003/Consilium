import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ClientService } from '../../services/client.service';
import { Client } from '../../models/client';

import { debounceTime } from 'rxjs/internal/operators/debounceTime';
import { Subject } from 'rxjs';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-clients',
  standalone: true,
  templateUrl: './clients.html',
  styleUrls: ['./clients.css'],
  imports: [PageTitleComponent, CommonModule, ButtonComponent, FormsModule, RouterLink],
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
  pageSize = 2;
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

  get visiblePages(): (number | string)[] {
    const total = this.totalPages;
    const current = this.currentPage;
    const delta = 2;
    const range: (number | string)[] = [];

    // Always show first and last page
    const left = Math.max(2, current - delta);
    const right = Math.min(total - 1, current + delta);

    range.push(1);

    if (left > 2) range.push('...');
    for (let i = left; i <= right; i++) range.push(i);
    if (right < total - 1) range.push('...');

    if (total > 1) range.push(total);
    return range;
  }

  onPageClick(page: number | string): void {
    if (page === '...') return;
    this.goToPage(Number(page));
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