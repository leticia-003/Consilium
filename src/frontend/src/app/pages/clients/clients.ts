import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-clients',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="Clients"></app-page-title>
      <p>Clients page placeholder.</p>
    </section>
  `,
  styleUrls: ['./clients.css']
  ,
  imports: [PageTitleComponent]
})
export class ClientsComponent {}
