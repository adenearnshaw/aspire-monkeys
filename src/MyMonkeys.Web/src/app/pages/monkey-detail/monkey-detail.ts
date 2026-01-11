import { Component, inject } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map, switchMap } from 'rxjs';

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
    switchMap((params) => this.monkeysApi.get(params.get('id') ?? '')),
    map((m) => ({
      ...m,
      mapUrl: this.buildOsmStaticMapUrl(m.latitude, m.longitude),
      osmLink: this.buildOsmLink(m.latitude, m.longitude),
    }))
  );

  private buildOsmStaticMapUrl(latitude: number, longitude: number): string | null {
    if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) return null;

    const zoom = 2
    const width = 560;
    const height = 320;

    const params = new URLSearchParams({
      lat: String(latitude),
      lon: String(longitude),
      zoom: String(zoom),
      width: String(width),
      height: String(height),
    });

    // Served by the Web BFF and rendered from official OpenStreetMap tiles.
    return `/api/map?${params.toString()}`;
  }

  private buildOsmLink(latitude: number, longitude: number): string {
    const lat = Number.isFinite(latitude) ? latitude : 0;
    const lon = Number.isFinite(longitude) ? longitude : 0;
    const zoom = 8;
    return `https://www.openstreetmap.org/?mlat=${encodeURIComponent(String(lat))}&mlon=${encodeURIComponent(String(lon))}#map=${zoom}/${encodeURIComponent(String(lat))}/${encodeURIComponent(String(lon))}`;
  }

}
