import { handleSession } from 'svelte-kit-cookie-session';
import type DecodedToken from '$lib/models/DecodedToken';
import type UserSession from '$lib/models/UserSession';
import { TOKEN_SECRET } from '$lib/utils/env';
import TokenInfo from '$lib/models/TokenInfo';
import jwt_decode from 'jwt-decode';

export const handle = handleSession(
    {
        secret: TOKEN_SECRET
    },
    async function ({ request, resolve }) {
        const response = await resolve(request);
        if (request.locals.session.data.tokens) {
            const data: UserSession = request.locals.session.data as UserSession;
            const decoded = jwt_decode(data.tokens.token) as DecodedToken;
            data.tokenInfo = new TokenInfo(decoded);
            request.locals.session.data = data;
            if (request.locals.session.data.expireIn) {
                request.locals.session.refresh(request.locals.session.data.expireIn);
            }
        }
        return response;
    }
);

export async function getSession({ locals }) {
    return locals.session.data;
}
