import { InjectionToken } from '@angular/core';

import { environment } from '../../../environments/environment';
import { AppEnvironment } from '../../../environments/environment.model';

/** Build-time environment, injectable so services stay testable. */
export const APP_ENVIRONMENT = new InjectionToken<AppEnvironment>('APP_ENVIRONMENT', {
  providedIn: 'root',
  factory: () => environment,
});
