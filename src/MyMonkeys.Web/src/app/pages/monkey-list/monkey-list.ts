import { Component, inject } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { combineLatest, map, startWith } from 'rxjs';

import { Monkeys, type Monkey } from '../../services/monkeys';

@Component({
  selector: 'app-monkey-list',
  imports: [AsyncPipe, NgFor, NgIf, RouterLink, ReactiveFormsModule],
  templateUrl: './monkey-list.html',
  styleUrl: './monkey-list.scss',
})
export class MonkeyList {
  private readonly monkeysApi = inject(Monkeys);
  readonly query = new FormControl('', { nonNullable: true });
  readonly monkeys$ = this.monkeysApi.list();
  readonly filteredMonkeys$ = combineLatest([
    this.monkeys$,
    this.query.valueChanges.pipe(startWith(this.query.value)),
  ]).pipe(
    map(([monkeys, query]) => filterMonkeys(monkeys, query))
  );

}

function filterMonkeys(monkeys: Monkey[], query: string): Monkey[] {
  const term = query.trim().toLowerCase();
  if (!term) return monkeys;
  return monkeys.filter((m) => {
    return (
      m.name.toLowerCase().includes(term) ||
      m.location.toLowerCase().includes(term) ||
      m.id.toLowerCase().includes(term)
    );
  });
}
