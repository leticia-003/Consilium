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
      <app-page-title title="Clients"></app-page-title>

      <div class="clients-toolbar">
        <input class="search" type="search" placeholder="Search by name..." (input)="onSearch($event.target.value)" />
      </div>

      <div *ngIf="loading" class="loading">Loading clients...</div>
      <div *ngIf="errorMessage" class="error">{{ errorMessage }}</div>

      <div *ngIf="!loading && !errorMessage">
        <table class="clients-table" *ngIf="filteredClients.length > 0">
          <thead>
            <tr>
              <th class="col-account">Account</th>
              <th class="col-email">Email</th>
              <th class="col-nif">NIF</th>
              <th class="col-date">Date</th>
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
        this.filteredClients = [...this.clients];
        this.loading = false;
        // Força a deteção de mudanças
        try { this.cdr.detectChanges(); } catch (e) { /* noop */ }
      },
      error: (err) => {
        console.error('Failed to load clients', err);
        this.errorMessage = 'Erro ao carregar clientes.';
        this.loading = false;
        try { this.cdr.detectChanges(); } catch (e) { /* noop */ }
      }
    });
  }

  onSearch(term: string): void {
    const q = (term || '').trim().toLowerCase();
    if (!q) {
      this.filteredClients = [...this.clients];
      return;
    }
    this.filteredClients = this.clients.filter(c => (c.name || '').toLowerCase().includes(q));
  }
}
