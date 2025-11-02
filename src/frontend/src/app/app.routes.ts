import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home';
import { ClientsComponent } from './pages/clients';
import { LawyersComponent } from './pages/lawyers';
import { ChatbotComponent } from './pages/chatbot';
import { CasesComponent } from './pages/cases';
import { ProfilesComponent } from './pages/profiles';
import { SettingsComponent } from './pages/settings';

export const routes: Routes = [
	{ path: '', component: HomeComponent, title: 'Home' },
	{ path: 'clients', component: ClientsComponent, title: 'Clients' },
	{ path: 'lawyers', component: LawyersComponent, title: 'Lawyers' },
	{ path: 'chatbot', component: ChatbotComponent, title: 'ChatBot' },
	{ path: 'cases', component: CasesComponent, title: 'Cases' },
	{ path: 'profiles', component: ProfilesComponent, title: 'Profiles' },
	{ path: 'settings', component: SettingsComponent, title: 'Settings' },
	{ path: '**', redirectTo: '' }
];
