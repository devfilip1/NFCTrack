import { environment } from '../../../environments/environment';
import { Link } from './link.models';

/** Host of the redirect service without the scheme, e.g. "localhost:8080". */
export const shortLinkHost = environment.shortLinkBaseUrl.replace(/^https?:\/\//, '');

export function shortUrl(link: Pick<Link, 'slug'>): string {
  return `${environment.shortLinkBaseUrl}/${link.slug}`;
}

/** Splits a URL into a readable host and the rest, so the host can be emphasised in the UI. */
export function splitUrl(url: string): { host: string; rest: string } {
  try {
    const parsed = new URL(url);
    const rest = `${parsed.pathname}${parsed.search}`;
    return { host: parsed.host.replace(/^www\./, ''), rest: rest === '/' ? '' : rest };
  } catch {
    return { host: url, rest: '' };
  }
}
