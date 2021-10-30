import { getAuthed } from '$lib/sorFetcher';
import type UserSession from '$lib/models/UserSession';
import type User from '$lib/types/user';

export async function get({ locals }) {
    if (locals.session.data) {
        const session: UserSession = locals.session.data;
        const newUserResponse = await getAuthed<User>('/auth/@me', session.tokens);
        if (newUserResponse.data) {
            locals.session.data.user = newUserResponse.data;
        }
    }

    return {
        body: {
            ok: true
        }
    };
}
