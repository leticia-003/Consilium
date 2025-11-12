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
  sortBy: 'name' | 'nif' | 'createdAt' | null = null;
  sortDir: 'asc' | 'desc' = 'asc';
  currentPage = 1;
  totalPages = 1;
  pageSize = 2;
  totalCount = 0;
  activeCount: number | null = null;
  inactiveCount: number | null = null;
  goToPageNumber = 1;

  private searchSubject = new Subject<string>();
  private searchSubscription: any;

  constructor(private lawyerService: LawyerService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject
      .pipe(debounceTime(500))
      .subscribe((term: string) => this.fetchLawyers(term));

    this.loadStatusCounts();

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

  ariaSort(column: 'name' | 'nif' | 'createdAt') {
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
        this.totalPages = Math.ceil(this.totalCount / this.pageSize);

        this.filteredLawyers = [...this.lawyers];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  private loadStatusCounts(): void {
  const setActive = (count: number) => { this.activeCount = count; this.cdr.detectChanges(); };
  const setInactive = (count: number) => { this.inactiveCount = count; this.cdr.detectChanges(); };

    this.lawyerService.getLawyers({ status: 'active', page: 1, limit: 1 }).subscribe({
      next: (res: any) => {
        const cnt = res?.meta?.totalCount ?? (Array.isArray(res) ? res.length : undefined);
        if (typeof cnt === 'number') {
          setActive(cnt);
        } else {
          this.lawyerService.getLawyers({ status: 'active', page: 1, limit: 1000 }).subscribe({
            next: (r2: any) => {
              const computed = (Array.isArray(r2) ? r2.length : r2?.data?.length);
              if (typeof computed === 'number') setActive(computed);
            },
            error: () => {
            }
          });
        }
      },
      error: () => {
      }
    });

    this.lawyerService.getLawyers({ status: 'inactive', page: 1, limit: 1 }).subscribe({
      next: (res: any) => {
        const cnt = res?.meta?.totalCount ?? (Array.isArray(res) ? res.length : undefined);
        if (typeof cnt === 'number') {
          setInactive(cnt);
        } else {
          this.lawyerService.getLawyers({ status: 'inactive', page: 1, limit: 1000 }).subscribe({
            next: (r2: any) => {
              const computed = (Array.isArray(r2) ? r2.length : r2?.data?.length);
              if (typeof computed === 'number') setInactive(computed);
            },
            error: () => {
            }
          });
        }
      },
      error: () => {
      }
    });

    setTimeout(() => {
      if (this.activeCount === null) {
        this.lawyerService.getLawyers({ status: 'active', page: 1, limit: 1 }).subscribe({
          next: (res: any) => {
            const cnt = res?.meta?.totalCount ?? (Array.isArray(res) ? res.length : undefined);
            if (typeof cnt === 'number') setActive(cnt);
          }
        });
      }
      if (this.inactiveCount === null) {
        this.lawyerService.getLawyers({ status: 'inactive', page: 1, limit: 1 }).subscribe({
          next: (res: any) => {
            const cnt = res?.meta?.totalCount ?? (Array.isArray(res) ? res.length : undefined);
            if (typeof cnt === 'number') setInactive(cnt);
          }
        });
      }
    }, 500);
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
      nif: l.nif,
      professionalRegister: l.professionalRegister,
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

  toggleSort(column: 'name' | 'nif' | 'createdAt') {
    if (this.sortBy === column) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDir = 'asc';
    }

    const dir = this.sortDir === 'asc' ? 1 : -1;
    this.filteredLawyers.sort((a: any, b: any) => {
      let res = 0;
      if (column === 'name') res = a.name.localeCompare(b.name);
      if (column === 'nif') res = Number(a.nif) - Number(b.nif);
      if (column === 'createdAt') res = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      return res * dir;
    });
  }

  get totalClients(): number { return this.totalCount; }
  get activeClients(): number | null { return this.activeCount; }
  get inactiveClients(): number | null { return this.inactiveCount; }
}
