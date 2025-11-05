import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ConfirmModalComponent } from '../../shared/confirm-modal/confirm-modal';
import { Router } from '@angular/router';
import { ClientService } from '../../services/client.service';
import { BreadcrumbService } from '../../shared/breadcrumb/breadcrumb.service';

@Component({
  selector: 'app-client-details',
  standalone: true,
  templateUrl: './client-details.html',
  styleUrls: ['./client-details.css'],
  imports: [CommonModule, PageTitleComponent, ButtonComponent, ConfirmModalComponent],
})
export class ClientDetailsComponent implements OnInit, OnDestroy {
  client: any = null;
  loading = false;
  error = '';
  showDeleteModal = false;
  // unavailable values (backend doesn't provide these yet)
  totalProcesses: number | null = null;
  lastActivity: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private clientService: ClientService,
    private breadcrumbService: BreadcrumbService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'No client id provided.';
      return;
    }

    this.loadClient(id);
  }

  get initials(): string {
    if (!this.client?.name) return '';
    return this.client.name
      .split(' ')
      .map((s: string) => s.charAt(0))
      .join('')
      .slice(0, 2)
      .toUpperCase();
  }

  loadClient(id: string) {
    this.loading = true;
    this.error = '';
    this.clientService.getClient(id).subscribe({
      next: (data: any) => {
        this.client = data;
        this.client.isActive = (data?.status ?? '').toString().toUpperCase() === 'ACTIVE';
        try {
          const url = `/clients/${id}`;
          if (this.client?.name) this.breadcrumbService.setLabelOverride(url, this.client.name);
        } catch (e) {
          // ignore
        }

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load client', err);
        this.error = 'Failed to load client details.';
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.breadcrumbService.clearLabelOverride(`/clients/${id}`);
    }
  }
  
  onDelete() {
    this.showDeleteModal = true;
  }

  confirmDelete() {
    if (!this.client?.id) return;
    this.loading = true;
    this.clientService.deleteClient(this.client.id).subscribe({
      next: () => {
        this.router.navigate(['/clients']);
      },
      error: (err) => {
        console.error('Failed to delete client', err);
        this.error = 'Failed to delete client.';
        this.loading = false;
        this.showDeleteModal = false;
      }
    });
  }
}
