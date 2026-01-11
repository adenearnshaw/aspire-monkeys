import { Routes } from '@angular/router';

import { MonkeyList } from './pages/monkey-list/monkey-list';
import { MonkeyDetail } from './pages/monkey-detail/monkey-detail';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'monkeys' },
	{ path: 'monkeys', component: MonkeyList },
	{ path: 'monkeys/:id', component: MonkeyDetail },
	{ path: '**', redirectTo: 'monkeys' },
];
