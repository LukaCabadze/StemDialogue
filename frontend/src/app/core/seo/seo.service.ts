import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

import { APP_ENVIRONMENT } from '../config/app-environment.token';
import { LanguageService } from '../i18n/language.service';
import { SeoDto } from '../models';

export interface PageSeo {
  readonly title: string;
  readonly description?: string | null;
  readonly imageUrl?: string | null;
  readonly canonicalPath?: string | null;
}

/** Writes title, description, Open Graph and canonical tags for the current page. */
@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly document = inject(DOCUMENT);
  private readonly environment = inject(APP_ENVIRONMENT);
  private readonly language = inject(LanguageService);

  apply(seo: SeoDto | PageSeo): void {
    const siteName = this.language.text().brand.name;
    const fullTitle = seo.title.includes(siteName) ? seo.title : `${seo.title} · ${siteName}`;
    const description = seo.description ?? '';
    const canonical = seo.canonicalPath ? this.absolute(seo.canonicalPath) : null;
    const image = seo.imageUrl ? this.absolute(seo.imageUrl) : null;

    this.title.setTitle(fullTitle);
    this.setName('description', description);
    this.setProperty('og:title', fullTitle);
    this.setProperty('og:description', description);
    this.setProperty('og:type', 'website');
    this.setProperty('og:site_name', siteName);
    this.setProperty('og:locale', this.language.isGeorgian() ? 'ka_GE' : 'en_US');
    this.setName('twitter:card', image ? 'summary_large_image' : 'summary');

    if (image) {
      this.setProperty('og:image', image);
    } else {
      this.meta.removeTag("property='og:image'");
    }

    if (canonical) {
      this.setProperty('og:url', canonical);
    }
    this.setCanonical(canonical);
  }

  private absolute(pathOrUrl: string): string {
    if (/^https?:\/\//i.test(pathOrUrl)) {
      return pathOrUrl;
    }
    return `${this.environment.siteUrl.replace(/\/+$/, '')}/${pathOrUrl.replace(/^\/+/, '')}`;
  }

  private setName(name: string, content: string): void {
    this.meta.updateTag({ name, content });
  }

  private setProperty(property: string, content: string): void {
    this.meta.updateTag({ property, content });
  }

  private setCanonical(href: string | null): void {
    let link = this.document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!href) {
      link?.remove();
      return;
    }
    if (!link) {
      link = this.document.createElement('link');
      link.rel = 'canonical';
      this.document.head.appendChild(link);
    }
    link.href = href;
  }
}
