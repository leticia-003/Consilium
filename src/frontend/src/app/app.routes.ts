import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home';
import { ClientsComponent } from './pages/clients';
import { ClientDetailsComponent } from './pages/client-details/client-details';
import { LawyersComponent } from './pages/lawyers';
import { ChatbotComponent } from './pages/chatbot';
import { CasesComponent } from './pages/cases';
import { ProfilesComponent } from './pages/profiles';
import { SettingsComponent } from './pages/settings';
import { CreateClientComponent } from './pages/create-client/create-client';

export const routes: Routes = [
	{ path: '', component: HomeComponent, title: 'Home' },
	{ path: 'clients/:id', component: ClientDetailsComponent, title: 'Client Details' },
	{ path: 'clients', component: ClientsComponent, title: 'Clients' },
	{ path: 'lawyers', component: LawyersComponent, title: 'Lawyers' },
	{ path: 'chatbot', component: ChatbotComponent, title: 'ChatBot' },
	{ path: 'cases', component: CasesComponent, title: 'Cases' },
	{ path: 'profiles', component: ProfilesComponent, title: 'Profiles' },
	{ path: 'settings', component: SettingsComponent, title: 'Settings' },
	{ path: 'create-client', component: CreateClientComponent, title: 'Create Client' },
	{ path: '**', redirectTo: '' }
];
