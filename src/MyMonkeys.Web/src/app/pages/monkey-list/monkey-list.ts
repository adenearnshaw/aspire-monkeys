import { Component, inject } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';

import { Monkeys, type Monkey } from '../../services/monkeys';

@Component({
  selector: 'app-monkey-list',
  imports: [AsyncPipe, NgFor, NgIf, RouterLink],
  templateUrl: './monkey-list.html',
  styleUrl: './monkey-list.scss',
})
export class MonkeyList {
  private readonly monkeysApi = inject(Monkeys);
  readonly monkeys$ = this.monkeysApi.list();

}
