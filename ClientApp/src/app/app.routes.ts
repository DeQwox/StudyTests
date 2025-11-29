import { Routes } from '@angular/router';

import { Home } from './home/home';
import { TestListComponent } from './test-list/test-list';
import { TestCreateComponent } from './test-create/test-create';
import { TestDetailComponent } from './test-detail/test-detail';

export const routes: Routes = [
  { path: 'home', component: Home },
  { path: 'tests', component: TestListComponent },
  { path: 'tests/create', component: TestCreateComponent },
  { path: 'tests/:id', component: TestDetailComponent },
  
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: '**', component: Home }
];

