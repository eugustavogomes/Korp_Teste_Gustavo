import { Injectable, OnDestroy, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription, timer } from 'rxjs';
import { switchMap, catchError, of } from 'rxjs';
import { HEALTH_ENDPOINTS } from '../core/api-endpoints';

export type ServiceStatus = 'healthy' | 'degraded' | 'unhealthy' | 'unknown';

@Injectable({ providedIn: 'root' })
export class HealthService implements OnDestroy {
  private http = inject(HttpClient);
  private sub = new Subscription();
  private _polling = false;

  readonly estoqueStatus  = signal<ServiceStatus>('unknown');
  readonly faturamentoStatus = signal<ServiceStatus>('unknown');

  startPolling(): void {
    if (this._polling) return;
    this._polling = true;

    const INTERVAL_MS = 30_000;

    this.sub.add(
      timer(0, INTERVAL_MS).pipe(
        switchMap(() =>
          this.http.get<{ status: string }>(HEALTH_ENDPOINTS.estoque).pipe(
            catchError(() => of({ status: 'Unhealthy' }))
          )
        )
      ).subscribe(r => this.estoqueStatus.set(this.map(r.status)))
    );

    this.sub.add(
      timer(0, INTERVAL_MS).pipe(
        switchMap(() =>
          this.http.get<{ status: string }>(HEALTH_ENDPOINTS.faturamento).pipe(
            catchError(() => of({ status: 'Unhealthy' }))
          )
        )
      ).subscribe(r => this.faturamentoStatus.set(this.map(r.status)))
    );
  }

  stopPolling(): void {
    this.sub.unsubscribe();
    this.sub = new Subscription();
    this._polling = false;
  }

  private map(status: string): ServiceStatus {
    switch (status?.toLowerCase()) {
      case 'healthy':  return 'healthy';
      case 'degraded': return 'degraded';
      default:         return 'unhealthy';
    }
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
