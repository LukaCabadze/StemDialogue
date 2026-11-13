import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LanguageService } from '../../../core/i18n/language.service';
import { ResourceCardDto } from '../../../core/models';
import { CoverVariant } from '../../../core/util/cover-variant';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';
import { Icon } from '../../../shared/ui/icon/icon';
import { Pill } from '../../../shared/ui/pill/pill';
import { fileExtension, resourceIcon } from '../../../shared/ui/resource-item/resource-icon';

const VARIANT_BY_TYPE: Readonly<Record<ResourceCardDto['type'], CoverVariant>> = {
  Document: 'light',
  Presentation: 'blue',
  Video: 'blue',
  Photo: 'pink',
  Link: 'light',
  Other: 'light',
};

/** Resource library card: thumbnail (or type-coloured placeholder), type, title, description, source. */
@Component({
  selector: 'sd-resource-card',
  imports: [RouterLink, CoverArt, Icon, Pill],
  templateUrl: './resource-card.html',
  styleUrl: './resource-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourceCard {
  protected readonly text = inject(LanguageService).text;

  readonly resource = input.required<ResourceCardDto>();

  protected readonly icon = computed(() => resourceIcon(this.resource().type));
  protected readonly variant = computed(() => VARIANT_BY_TYPE[this.resource().type]);
  protected readonly extension = computed(() => fileExtension(this.resource().file?.contentType ?? this.resource().file?.url));
  protected readonly source = computed(() => this.resource().event?.title ?? this.resource().author?.fullName ?? this.resource().authorName);
}
