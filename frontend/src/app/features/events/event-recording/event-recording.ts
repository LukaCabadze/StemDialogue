import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { LanguageService } from '../../../core/i18n/language.service';
import { MediaDto } from '../../../core/models';
import { isTrustedEmbedUrl } from '../../../core/util/external-url';
import { CoverArt } from '../../../shared/ui/cover-art/cover-art';

/**
 * Meeting recording: an embedded player for trusted hosts (YouTube, Vimeo, Facebook),
 * otherwise the illustrated cover with a play button linking to the recording.
 */
@Component({
  selector: 'sd-event-recording',
  imports: [CoverArt],
  templateUrl: './event-recording.html',
  styleUrl: './event-recording.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EventRecording {
  private readonly sanitizer = inject(DomSanitizer);
  protected readonly text = inject(LanguageService).text;

  readonly title = input.required<string>();
  readonly recordingUrl = input<string | null>(null);
  readonly embedUrl = input<string | null>(null);
  readonly cover = input<MediaDto | null>(null);

  // Only allow-listed https players reach bypassSecurityTrustResourceUrl.
  protected readonly safeEmbed = computed<SafeResourceUrl | null>(() => {
    const url = this.embedUrl();
    return isTrustedEmbedUrl(url) ? this.sanitizer.bypassSecurityTrustResourceUrl(url) : null;
  });
}
