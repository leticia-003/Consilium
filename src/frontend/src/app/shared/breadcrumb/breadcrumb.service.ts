import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { BreadcrumbItem } from './breadcrumb.model';

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  private _crumbs = new BehaviorSubject<BreadcrumbItem[]>([]);
  readonly crumbs$ = this._crumbs.asObservable();
  // optional overrides for crumb labels by URL (useful for dynamic labels e.g. names)
  private overrides = new Map<string, string>();

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe(() => {
      this.build(this.route.root);
    });

    this.build(this.route.root);
  }

  setLabelOverride(url: string, label: string) {
    if (!url) return;
    this.overrides.set(url, label);
    // if crumbs already built, update immediately
    const current = this._crumbs.getValue();
    const found = current.find((c) => c.url === url);
    if (found) {
      found.label = label;
      this._crumbs.next([...current]);
    }
  }

  clearLabelOverride(url: string) {
    if (!url) return;
    this.overrides.delete(url);
  }

  private build(route: ActivatedRoute) {
    const crumbs: BreadcrumbItem[] = [];
    this.addCrumbs(route, '', crumbs);

    // Home page has no breadcrumb

    if (!crumbs.find((c) => c.url === '/')) {
      crumbs.unshift({ label: 'Consilium', url: '/' });
    }

    const currentUrl = this.router.url || '';
    // Home page has no breadcrumb
    if (currentUrl === '/' || currentUrl === '') {
      this._crumbs.next([]);
      return;
    }
    const segments = currentUrl.split('/').filter(Boolean);
    if (segments.length >= 2) {
      const parentUrl = `/${segments[0]}`;
      if (!crumbs.find((c) => c.url === parentUrl)) {

        const cfg = this.router.config.find((r) => r.path === segments[0]);
        const parentLabel = (cfg && (cfg['title'] || cfg.data?.['breadcrumb'] || cfg.data?.['title'])) ?? this.titleize(segments[0]);

        const insertIndex = Math.max(1, crumbs.length - 1);
        crumbs.splice(insertIndex, 0, { label: parentLabel ?? '', url: parentUrl });
      }
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

      const override = this.overrides.get(nextUrl);
      const finalLabel = override ?? ((label as string) ?? '');

      this.setCrumb(nextUrl, finalLabel, crumbs);

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
    
    const isUuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(seg);
    const isLongHex = /^[0-9a-f]{24}$/i.test(seg);
    const isNumeric = /^\d+$/.test(seg);
    if (isUuid || isLongHex || isNumeric) return '';

    return seg
      .replace(/[-_]/g, ' ')
      .replace(/\b\w/g, (l) => l.toUpperCase());
  }
}
