export interface AppEnvironment {
  readonly production: boolean;
  /** Base URL of the public REST API, without a trailing slash. */
  readonly apiBaseUrl: string;
  /** Project opened when a visitor lands on the site root. */
  readonly defaultProjectSlug: string;
  /** Absolute origin used for canonical links and share URLs. */
  readonly siteUrl: string;
}
