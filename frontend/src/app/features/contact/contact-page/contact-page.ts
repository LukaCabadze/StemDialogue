import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { PagesApi } from '../../../core/api/pages.api';
import { LanguageService } from '../../../core/i18n/language.service';
import { ContactPageDto } from '../../../core/models';
import { ProjectContext } from '../../../core/project/project-context.service';
import { SeoService } from '../../../core/seo/seo.service';
import { ErrorState } from '../../../shared/ui/error-state/error-state';
import { Icon } from '../../../shared/ui/icon/icon';
import { IconName } from '../../../shared/ui/icon/icon-paths';
import { LoadingState } from '../../../shared/ui/loading-state/loading-state';
import { PageHero } from '../../../shared/ui/page-hero/page-hero';
import { PartnerGroups } from '../../../shared/ui/partner-groups/partner-groups';
import { RichText } from '../../../shared/ui/rich-text/rich-text';

interface ContactLine {
  readonly icon: IconName;
  readonly label: string;
  readonly value: string;
  readonly href: string | null;
}

/** Contact details, social links, CMS text and the partner organisations. */
@Component({
  selector: 'sd-contact-page',
  imports: [ErrorState, Icon, LoadingState, PageHero, PartnerGroups, RichText],
  templateUrl: './contact-page.html',
  styleUrl: './contact-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContactPage {
  private readonly api = inject(PagesApi);
  private readonly seo = inject(SeoService);
  private readonly languages = inject(LanguageService);
  private readonly project = inject(ProjectContext);
  protected readonly text = this.languages.text;

  protected readonly contact = rxResource({
    params: () => ({ slug: this.project.slug(), language: this.languages.language() }),
    stream: ({ params }) => this.api.getContact(params.slug),
  });

  protected readonly data = computed<ContactPageDto | null>(() => (this.contact.hasValue() ? this.contact.value() : null));

  protected readonly lines = computed<readonly ContactLine[]>(() => {
    const contact = this.data()?.contact;
    const copy = this.text().contact;
    if (!contact) {
      return [];
    }
    const lines: ContactLine[] = [];
    if (contact.person) {
      lines.push({ icon: 'users', label: copy.person, value: contact.person, href: null });
    }
    if (contact.email) {
      lines.push({ icon: 'mail', label: copy.email, value: contact.email, href: `mailto:${contact.email}` });
    }
    if (contact.phone) {
      lines.push({ icon: 'phone', label: copy.phone, value: contact.phone, href: `tel:${contact.phone.replace(/\s+/g, '')}` });
    }
    if (contact.address) {
      lines.push({ icon: 'map-pin', label: copy.address, value: contact.address, href: null });
    }
    return lines;
  });

  protected readonly crumbs = computed(() => [
    { label: this.text().nav.home, path: this.project.rootPath() },
    { label: this.data()?.page.title ?? this.text().contact.title },
  ]);

  constructor() {
    effect(() => {
      const contact = this.data();
      if (contact) {
        this.seo.apply(contact.page.seo);
      }
    });
  }
}
