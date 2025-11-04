import { Component } from '@angular/core';
import { PageTitleComponent } from '../../shared/page-title/page-title';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  templateUrl: './chatbot.html',
  styleUrls: ['./chatbot.css'],
  imports: [PageTitleComponent]
})
export class ChatbotComponent {}
