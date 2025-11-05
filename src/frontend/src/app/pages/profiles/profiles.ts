import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-profiles',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Profiles"></app-page-title>
      <p>Profiles page placeholder.</p>
    </section>
  `,
  styleUrls: ['./profiles.css'],
  imports: [PageTitleComponent]
})
export class ProfilesComponent {}
