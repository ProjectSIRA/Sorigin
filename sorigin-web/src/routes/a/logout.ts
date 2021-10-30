import type UserSession from '$lib/models/UserSession';
import type { Request } from '@sveltejs/kit';
import axios from 'axios';
import { soriginURL } from '$lib/sorigin';

export async function del({ locals }: Request) {
    if (locals.session.data?.tokens) {
        const session: UserSession = locals.session.data;
        try {
            await axios.delete(soriginURL() + '/api/auth/refresh' + session.tokens.refreshToken);
        } catch {}
    }

    locals.session.destroy();

    return {
        body: {
            ok: true
        }
    };
}
