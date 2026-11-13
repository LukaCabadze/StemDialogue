import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../core/i18n/language.service';
import { ProjectContext } from '../../core/project/project-context.service';
import { SiteStore } from '../../core/site/site.store';

interface NamePart {
  readonly name: string;
  /** Text glued directly after the bold name (", ", " და ", "-ს პარტნიორობით"). */
  readonly after: string;
}

/**
 * 38px band under the header: "პროექტი ხორციელდება **[funder]** დაფინანსებით • **ბტუ** და **აწსუ**-ს
 * პარტნიორობით · დეტალურად →". Funder and partners stay visually separate roles.
 */
@Component({
  selector: 'sd-funding-strip',
  imports: [RouterLink],
  templateUrl: './funding-strip.html',
  styleUrl: './funding-strip.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FundingStrip {
  private readonly site = inject(SiteStore);
  private readonly project = inject(ProjectContext);
  protected readonly text = inject(LanguageService).text;

  protected readonly aboutPath = computed(() => this.project.path('about'));
  protected readonly funderParts = computed(() =>
    this.join(
      this.site.funders().map((partner) => partner.name),
      '',
    ),
  );
  protected readonly partnerParts = computed(() =>
    this.join(
      this.site.partners().map((partner) => partner.shortName),
      this.text().funding.partnershipSuffix,
    ),
  );
  protected readonly fallback = computed(() => this.site.funding()?.shortText ?? '');

  /** ["ბტუ", "აწსუ"] → ბტუ" და " აწსუ"-ს პარტნიორობით" — punctuation follows each name without a gap. */
  private join(names: readonly string[], suffix: string): readonly NamePart[] {
    const and = ` ${this.text().funding.and} `;
    return names.map((name, index) => ({
      name,
      after: index === names.length - 1 ? suffix : index === names.length - 2 ? and : ', ',
    }));
  }
}
