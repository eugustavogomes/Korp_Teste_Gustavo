import { Component, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { ServiceStatus } from '../../services/health.service';

@Component({
  selector: 'app-status-indicator',
  standalone: true,
  imports: [TooltipModule],
  template: `
    <span class="status-indicator"
          [class]="'status-indicator--' + status()"
          [pTooltip]="label()"
          tooltipPosition="bottom">
    </span>
  `,
  styleUrl: './status-indicator.component.scss',
})
export class StatusIndicatorComponent {
  status = input.required<ServiceStatus>();
  label  = input.required<string>();
}
