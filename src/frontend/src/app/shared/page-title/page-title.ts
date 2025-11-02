import { Component, Input, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-title',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="page-title">
      <div class="page-title-main">
        <h1 class="page-title-heading">{{ title }}</h1>
        <p *ngIf="subtitle" class="page-title-sub">{{ subtitle }}</p>
      </div>

      <div *ngIf="actions" class="page-title-actions" aria-hidden="false">
        <ng-container *ngTemplateOutlet="actions"></ng-container>
      </div>
    </header>
  `,
  styleUrls: ['./page-title.css']
})
export class PageTitleComponent {
  @Input() title!: string;
  @Input() subtitle?: string;
  @Input() actions?: TemplateRef<any> | null;
}
