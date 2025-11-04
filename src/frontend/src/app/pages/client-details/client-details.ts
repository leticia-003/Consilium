import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { PageTitleComponent } from '../../shared/page-title/page-title';
import { ButtonComponent } from '../../shared/button/button';
import { ClientService } from '../../services/client.service';
import { BreadcrumbService } from '../../shared/breadcrumb/breadcrumb.service';

@Component({
  selector: 'app-client-details',
  standalone: true,
  templateUrl: './client-details.html',
  styleUrls: ['./client-details.css'],
  imports: [CommonModule, PageTitleComponent, ButtonComponent],
})
export class ClientDetailsComponent implements OnInit, OnDestroy {
  client: any = null;
  loading = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private clientService: ClientService,
    private breadcrumbService: BreadcrumbService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'No client id provided.';
      return;
    }

    this.loadClient(id);
  }

  loadClient(id: string) {
    this.loading = true;
    this.error = '';
    this.clientService.getClient(id).subscribe({
      next: (data: any) => {
        this.client = data;
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
}
