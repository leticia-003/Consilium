import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './pages/home';
import { ClientsComponent } from './pages/clients';
import { ClientDetailsComponent } from './pages/client-details/client-details';
import { LawyersComponent } from './pages/lawyers';
import { ChatbotComponent } from './pages/chatbot';
import { CasesComponent } from './pages/cases';
import { ProfilesComponent } from './pages/profiles';
import { SettingsComponent } from './pages/settings';
import { CreateClientComponent } from './pages/create-client/create-client';
import { LoginComponent } from './pages/login/login';
import { NgModule } from '@angular/core';
import { EditClientComponent } from './pages/edit-client/edit-client';

export const routes: Routes = [
	{ path: '', component: LoginComponent, title: 'Login' },
	{ path: 'home', component: HomeComponent, title: 'Home' },
	{ path: 'clients/:id', component: ClientDetailsComponent, title: 'Client Details' },
	{ path: 'clients/:id/edit', component: EditClientComponent, title: 'Edit Client' },
	{ path: 'clients', component: ClientsComponent, title: 'Clients' },
	{ path: 'lawyers', component: LawyersComponent, title: 'Lawyers' },
	{ path: 'chatbot', component: ChatbotComponent, title: 'ChatBot' },
	{ path: 'cases', component: CasesComponent, title: 'Cases' },
	{ path: 'profiles', component: ProfilesComponent, title: 'Profiles' },
	{ path: 'settings', component: SettingsComponent, title: 'Settings' },
	{ path: 'create-client', component: CreateClientComponent, title: 'Create Client' },
	{ path: 'login', component: LoginComponent },
	{ path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})

export class AppRoutingModule { }
