import { ResourceType } from '../../../core/models';
import { IconName } from '../icon/icon-paths';

const ICONS: Readonly<Record<ResourceType, IconName>> = {
  Document: 'file-text',
  Presentation: 'presentation',
  Video: 'video',
  Photo: 'image',
  Link: 'link',
  Other: 'file-text',
};

export function resourceIcon(type: ResourceType): IconName {
  return ICONS[type];
}

/** "application/pdf" or ".../slides.pptx" → "PDF" / "PPTX". */
export function fileExtension(contentTypeOrUrl: string | null | undefined): string {
  if (!contentTypeOrUrl) {
    return '';
  }
  if (contentTypeOrUrl.includes('pdf')) {
    return 'PDF';
  }
  const match = /\.([a-z0-9]{2,5})(?:[?#].*)?$/i.exec(contentTypeOrUrl);
  return match?.[1]?.toUpperCase() ?? '';
}
