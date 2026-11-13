import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { ApiClient } from '../api/api-client.service';
import { LanguageService } from '../i18n/language.service';

/** Adds `?lang=` to every public API call so content comes back in the visitor's language. */
export const languageInterceptor: HttpInterceptorFn = (request, next) => {
  const api = inject(ApiClient);
  if (!api.isApiUrl(request.url) || request.params.has('lang')) {
    return next(request);
  }

  const language = inject(LanguageService).language();
  return next(request.clone({ setParams: { lang: language } }));
};
