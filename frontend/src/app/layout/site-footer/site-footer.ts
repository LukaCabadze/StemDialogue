import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { APP_ENVIRONMENT } from '../../core/config/app-environment.token';
import { LanguageService } from '../../core/i18n/language.service';
import { ProjectContext } from '../../core/project/project-context.service';
import { SiteStore } from '../../core/site/site.store';
import { PartnerCard } from '../../shared/ui/partner-card/partner-card';

interface FooterLink {
  readonly label: string;
  readonly path: string;
}

/** Funding band with logos, four link columns and the copyright bar. */
@Component({
  selector: 'sd-site-footer',
  imports: [RouterLink, PartnerCard],
  templateUrl: './site-footer.html',
  styleUrl: './site-footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteFooter {
  private readonly project = inject(ProjectContext);
  private readonly siteUrl = inject(APP_ENVIRONMENT).siteUrl;
  protected readonly site = inject(SiteStore);
  protected readonly text = inject(LanguageService).text;

  protected readonly year = new Date().getFullYear();
  protected readonly homePath = this.project.rootPath;
  protected readonly logos = computed(() => [...this.site.funders(), ...this.site.partners()]);
  protected readonly displayUrl = computed(
    () => `${this.siteUrl.replace(/^https?:\/\//, '')}${this.project.rootPath()}`,
  );

  protected readonly projectLinks = computed<readonly FooterLink[]>(() => {
    const nav = this.text().nav;
    return [
      { label: nav.about, path: this.project.path('about') },
      { label: nav.stemSaturdays, path: this.project.path('stem-saturdays') },
      { label: nav.news, path: this.project.path('news') },
      { label: nav.ambassadors, path: this.project.path('stem-ambassadors') },
      { label: nav.resources, path: this.project.path('resources') },
    ];
  });

  protected readonly peopleLinks = computed<readonly FooterLink[]>(() => {
    const nav = this.text().nav;
    return [
      { label: nav.team, path: this.project.path('team') },
      { label: nav.researchers, path: this.project.path('researchers') },
      { label: nav.mentors, path: this.project.path('mentors') },
    ];
  });
}
