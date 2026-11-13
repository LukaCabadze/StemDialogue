import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { ResourceCardDto } from '../../../core/models';
import { Icon } from '../icon/icon';
import { fileExtension, resourceIcon } from './resource-icon';

export type ResourceItemTone = 'light' | 'dark';

/** One material (slides, recording, photos, link) as a compact row with a type icon. */
@Component({
  selector: 'sd-resource-item',
  imports: [RouterLink, Icon],
  templateUrl: './resource-item.html',
  styleUrl: './resource-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.tone-dark]': "tone() === 'dark'",
  },
})
export class ResourceItem {
  readonly resource = input.required<ResourceCardDto>();
  readonly tone = input<ResourceItemTone>('light');

  protected readonly icon = computed(() => resourceIcon(this.resource().type));
  protected readonly meta = computed(() => {
    const resource = this.resource();
    const extension = fileExtension(resource.file?.contentType ?? null) || fileExtension(resource.file?.url ?? null);
    return [resource.typeLabel, extension || resource.event?.title || resource.authorName]
      .filter((part): part is string => !!part)
      .join(' · ');
  });
}
