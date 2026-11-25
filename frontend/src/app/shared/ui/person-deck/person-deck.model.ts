import { MediaDto } from '../../../core/models';

/** One card in a person deck. */
export interface DeckPerson {
  readonly key: string;
  readonly name: string;
  readonly photo: MediaDto | null;
  /** Big heading on the card (role title). */
  readonly title: string | null;
  /** Small line under the name (organisation). */
  readonly meta: string | null;
  /** Main text on the right (short bio or title · organisation). */
  readonly text: string | null;
  readonly path: string | null;
}
