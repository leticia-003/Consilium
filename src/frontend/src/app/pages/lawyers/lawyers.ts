import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { LawyerService } from '../../services/lawyer.service';
import { Lawyer } from '../../models/lawyer';

import { debounceTime } from 'rxjs/internal/operators/debounceTime';
import { Subject } from 'rxjs';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-lawyers',
  standalone: true,
  templateUrl: './lawyers.html',
  styleUrls: ['./lawyers.css'],
  imports: [PageTitleComponent, CommonModule, ButtonComponent, FormsModule, RouterLink],
})

export class LawyersComponent implements OnInit, OnDestroy {
  lawyers: Lawyer[] = [];
  filteredLawyers: Lawyer[] = [];
  loading = false;
  errorMessage = '';
  selectedFilter: 'all' | 'active' | 'inactive' = 'all';
  searchTerm = '';
  sortBy: 'name' | 'register' | 'createdAt' | null = null;
  sortDir: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  totalPages = 1;
  pageSize = 2;
  totalCount = 0;
  goToPageNumber = 1;

  private searchSubject = new Subject<string>();
  private searchSubscription: any;

  constructor(private lawyerService: LawyerService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject
      .pipe(debounceTime(500))
      .subscribe((term: string) => this.fetchLawyers(term));

    this.loadLawyers();
  }

  loadLawyers(page = 1): void {
    this.loading = true;
    this.errorMessage = '';
    this.currentPage = page;

    this.lawyerService.getLawyers({ page: this.currentPage, limit: this.pageSize }).subscribe({
      next: (data: any) => {
        const apiData = Array.isArray(data) ? data : data.data;
        this.lawyers = this.transformApiData(apiData);

  this.totalCount = data.meta?.totalCount || this.lawyers.length;
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        this.filteredLawyers = [...this.lawyers];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load lawyers', err);
        this.errorMessage = 'Failed to load lawyers.';
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchTerm = (term || '').trim();
    this.searchSubject.next(this.searchTerm);
  }

  ariaSort(column: 'name' | 'register' | 'createdAt') {
    if (this.sortBy !== column) return 'none';
    return this.sortDir === 'asc' ? 'ascending' : 'descending';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;

    if (this.searchTerm) {
      this.fetchLawyers(this.searchTerm, page);
    } else {
      this.loadLawyers(page);
    }
  }

  private fetchLawyers(term: string, page = 1): void {
    this.loading = true;
    this.currentPage = page;

    this.lawyerService.getLawyers({ search: term, page: this.currentPage, limit: this.pageSize }).subscribe({
      next: (data: any) => {
        const apiData = Array.isArray(data) ? data : data.data;
        this.lawyers = this.transformApiData(apiData);

    this.totalCount = data.meta?.totalCount || this.lawyers.length;
    this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        this.filteredLawyers = [...this.lawyers];
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


  private transformApiData(apiData: any[]): Lawyer[] {
    return (apiData || []).map((l: any) => ({
      id: l.id,
      name: l.name,
      email: l.email,
      professionalRegister: l.professionalRegister ?? l.nif,
      phone: l.phone,
      isActive: l.status?.toUpperCase() === 'ACTIVE',
      createdAt: l.createdAt || null
    }));
  }

  applyFilter(filter: 'all' | 'active' | 'inactive'): void {
    this.selectedFilter = filter;
    if (filter === 'active') {
      this.filteredLawyers = this.lawyers.filter(c => c.isActive);
    } else if (filter === 'inactive') {
      this.filteredLawyers = this.lawyers.filter(c => !c.isActive);
    } else {
      this.filteredLawyers = [...this.lawyers];
    }
  }

  ngOnDestroy(): void {
    if (this.searchSubscription) this.searchSubscription.unsubscribe();
  }

  toggleSort(column: 'name' | 'register' | 'createdAt') {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }

    const dir = this.sortDir === 'asc' ? 1 : -1;
    this.filteredLawyers.sort((a: any, b: any) => {
      let res = 0;
      if (column === 'name') {
        res = (a.name || '').localeCompare(b.name || '');
      }

      if (column === 'register') {
        const va = (a.professionalRegister ?? '') + '';
        const vb = (b.professionalRegister ?? '') + '';
        const na = Number(va.replace(/\D/g, ''));
        const nb = Number(vb.replace(/\D/g, ''));
        if (!isNaN(na) && !isNaN(nb) && va.trim() !== '' && vb.trim() !== '') {
          res = na - nb;
        } else {
          res = va.localeCompare(vb);
        }
      }

      if (column === 'createdAt') {
        res = (new Date(a.createdAt).getTime() || 0) - (new Date(b.createdAt).getTime() || 0);
      }

      return res * dir;
    });
  }

  get totalClients(): number { return this.lawyers.length; }
  get activeClients(): number { return this.lawyers.filter(l => !!l.isActive).length; }
  get inactiveClients(): number { return this.lawyers.filter(l => !l.isActive).length; }
}
