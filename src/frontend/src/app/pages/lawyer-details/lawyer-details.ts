import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ConfirmModalComponent } from '../../shared/confirm-modal/confirm-modal';
import { NotificationService } from '../../shared/notification/notification.service';
import { Router } from '@angular/router';
import { LawyerService } from '../../services/lawyer.service';
import { BreadcrumbService } from '../../shared/breadcrumb/breadcrumb.service';
import { getFlagEmoji, getDialPrefix } from '../../shared/phone-countries/phone-countries';


@Component({
  selector: 'app-lawyer-details',
  standalone: true,
  templateUrl: './lawyer-details.html',
  styleUrls: ['./lawyer-details.css'],
  imports: [CommonModule, PageTitleComponent, ButtonComponent, ConfirmModalComponent],
})
export class LawyerDetailsComponent implements OnInit, OnDestroy {
  lawyer: any = null;
  loading = false;
  error = '';
  showDeleteModal = false;
  // unavailable values (backend doesn't provide these yet)
  totalProcesses: number | null = null;
  lastActivity: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private lawyerService: LawyerService,
    private breadcrumbService: BreadcrumbService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'No lawyer id provided.';
      return;
    }

    this.loadLawyer(id);
  }

  getPhoneFlag(dialCode?: number | string | null) {
    return getFlagEmoji(dialCode);
  }

  getPhoneDialPrefix(dialCode?: number | string | null) {
    return getDialPrefix(dialCode);
  }

  get initials(): string {
    if (!this.lawyer?.name) return '';
    return this.lawyer.name
      .split(' ')
      .map((s: string) => s.charAt(0))
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  loadLawyer(id: string) {
    this.loading = true;
    this.error = '';
    this.lawyerService.getLawyer(id).subscribe({
      next: (data: any) => {
        this.lawyer = data;
        this.lawyer.isActive = (data?.status ?? '').toString().toUpperCase() === 'ACTIVE';
  this.lawyer.professionalRegister = this.lawyer.professionalRegister ?? data?.professionalRegister ?? '';

        try {
          const url = `/lawyers/${id}`;
          if (this.lawyer?.name) this.breadcrumbService.setLabelOverride(url, this.lawyer.name);
        } catch (e) {
          // ignore
        }

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load lawyer', err);
        this.error = 'Failed to load lawyer details.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.breadcrumbService.clearLabelOverride(`/lawyers/${id}`);
    }
  }
  
  onDelete() {
    this.showDeleteModal = true;
  }

  confirmDelete() {
        this.notificationService.showSuccess('Lawyer successfully deleted.', 3000);
        setTimeout(() => this.router.navigate(['/lawyers']), 800);
    this.lawyerService.deleteLawyer(this.lawyer.id).subscribe({
      next: () => {
        this.router.navigate(['/lawyers']);
      },
      error: (err) => {
        console.error('Failed to delete lawyer', err);
        this.error = 'Failed to delete lawyer.';
        this.loading = false;
        this.showDeleteModal = false;
      }
    });
  }
}
