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
  templateUrl: './breadcrumb.html',
  styleUrls: ['./breadcrumb.css']
})
export class BreadcrumbComponent {
  @Input() separator = '›';

  readonly crumbs$: Observable<BreadcrumbItem[]>;

  constructor(private breadcrumbService: BreadcrumbService) {
    this.crumbs$ = this.breadcrumbService.crumbs$;
  }
}
