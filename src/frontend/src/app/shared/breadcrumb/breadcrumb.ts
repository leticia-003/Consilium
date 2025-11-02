import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { BreadcrumbService } from './breadcrumb.service';
import { BreadcrumbItem } from './breadcrumb.model';

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <ng-container *ngIf="crumbs$ | async as crumbs">
      <nav *ngIf="crumbs.length > 0" class="breadcrumb" aria-label="Breadcrumb">
        <ol itemscope itemtype="https://schema.org/BreadcrumbList">
          <li *ngFor="let crumb of crumbs; let i = index; let last = last" [attr.itemprop]="'itemListElement'" itemscope itemtype="https://schema.org/ListItem">
            <a *ngIf="!last && crumb.label" [routerLink]="crumb.url" itemprop="item">
              <span itemprop="name">{{ crumb.label }}</span>
            </a>
            <span *ngIf="last || !crumb.label" class="current" aria-current="page"><span itemprop="name">{{ crumb.label }}</span></span>
            <meta itemprop="position" [attr.content]="i + 1" />
            <span *ngIf="!last" class="sep" aria-hidden="true">{{ separator }}</span>
          </li>
        </ol>
      </nav>
    </ng-container>
  `,
  styleUrls: ['./breadcrumb.css']
})
export class BreadcrumbComponent {
  @Input() separator = '›';

  readonly crumbs$: Observable<BreadcrumbItem[]>;

  constructor(private breadcrumbService: BreadcrumbService) {
    this.crumbs$ = this.breadcrumbService.crumbs$;
  }
}
