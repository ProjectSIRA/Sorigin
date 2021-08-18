import { REFETCH_URL, SORIGIN_URL } from './utils/env'

/** @type {import('@sveltejs/kit').ExternalFetch} */
export async function externalFetch(request: Request) {
    if (request.url.startsWith(SORIGIN_URL)) {
        request = new Request(request.url.replace(SORIGIN_URL, REFETCH_URL), request)
    }
    return fetch(request)
}