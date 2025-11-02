import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { BreadcrumbItem } from './breadcrumb.model';

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  private _crumbs = new BehaviorSubject<BreadcrumbItem[]>([]);
  readonly crumbs$ = this._crumbs.asObservable();

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe(() => {
      this.build(this.route.root);
    });

    this.build(this.route.root);
  }

  private build(route: ActivatedRoute) {
    const crumbs: BreadcrumbItem[] = [];
    this.addCrumbs(route, '', crumbs);

    // Home page has no breadcrumb
    const currentUrl = this.router.url || '';
    if (currentUrl === '/' || currentUrl === '') {
      this._crumbs.next([]);
      return;
    }

    if (!crumbs.find((c) => c.url === '/')) {
      crumbs.unshift({ label: 'Consilium', url: '/' });
    }

    // mark last as non-clickable
    crumbs.forEach((c, i) => (c.isClickable = i !== crumbs.length - 1));
    this._crumbs.next(crumbs);
  }

  private addCrumbs(route: ActivatedRoute, url: string, crumbs: BreadcrumbItem[]) {
    const children = route.children;
    for (const child of children) {
      if (child.outlet !== 'primary') continue;

      const routeURL = child.snapshot.url.map((s) => s.path).join('/');
      const nextUrl = routeURL ? `${url}/${routeURL}` : url;

      let label: unknown = child.snapshot.data?.['breadcrumb'] ?? child.snapshot.data?.['title'];

      if (typeof label !== 'string') {
        label = routeURL ? this.titleize(routeURL) : '';
      }

      this.setCrumb(nextUrl, (label as string) ?? '', crumbs);

      this.addCrumbs(child, nextUrl, crumbs);
    }
  }

  private setCrumb(url: string, label: string, crumbs: BreadcrumbItem[]) {
    const existing = crumbs.find((c) => c.url === url);
    if (existing) existing.label = label;
    else crumbs.push({ label: label ?? '', url });
  }

  private titleize(path: string) {
    const seg = path.split('/').pop() ?? path;
    return seg
      .replace(/[-_]/g, ' ')
      .replace(/\b\w/g, (l) => l.toUpperCase());
  }
}
