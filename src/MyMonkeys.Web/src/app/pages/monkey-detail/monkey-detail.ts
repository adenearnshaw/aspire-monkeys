import { Component, inject } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';

import { Monkeys } from '../../services/monkeys';

@Component({
  selector: 'app-monkey-detail',
  imports: [AsyncPipe, NgIf, RouterLink],
  templateUrl: './monkey-detail.html',
  styleUrl: './monkey-detail.scss',
})
export class MonkeyDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly monkeysApi = inject(Monkeys);

  readonly monkey$ = this.route.paramMap.pipe(
    switchMap((params) => this.monkeysApi.get(params.get('id') ?? ''))
  );

}
