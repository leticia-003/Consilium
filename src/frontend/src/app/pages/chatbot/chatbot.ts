import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  template: `
    <section class="page">
      <app-page-title title="ChatBot"></app-page-title>
      <p>ChatBot page placeholder.</p>
    </section>
  `,
  styleUrls: ['./chatbot.css'],
  imports: [PageTitleComponent]
})
export class ChatbotComponent {}
