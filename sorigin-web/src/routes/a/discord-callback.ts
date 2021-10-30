import type Tokens from '$lib/models/Tokens';
import { getRefreshTimeInDays } from '$lib/sorFetcher';
import { soriginURL } from '$lib/sorigin';
import type TokenSet from '$lib/types/tokenSet';
import type User from '$lib/types/user';
import { REFETCH_URL } from '$lib/utils/env';
import type { Request } from '@sveltejs/kit';
import axios from 'axios';
import fetch from 'node-fetch';

const tokenURL = `${REFETCH_URL}/api/auth/login`;
const userURL = `${REFETCH_URL}/api/auth/@me`;

async function getTokens(grant: string): Promise<TokenSet | null> {
    const res = await fetch(`${tokenURL}?grant=${grant}&platform=discord`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json'
        }
    });
    if (!res.ok) return null;
    const body = await res.json();
    return body as TokenSet;
}

async function getUser(accessToken: string): Promise<User | null> {
    const res = await fetch(userURL, {
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${accessToken}`
        }
    });
    if (!res.ok) return null;
    const body = await res.json();
    return body as User;
}

export async function get({ query, locals }: Request) {
    const code = query.get('grant');
    if (code) {
        try {
            const response = await axios.post<Tokens>(
                soriginURL() + '/api/auth/login?grant=' + code + '&platform=discord'
            );
            const tokens: Tokens = response.data;

            const timeInDays = await getRefreshTimeInDays();

            const userResponse = await axios.get<User>(soriginURL() + '/api/auth/@me', {
                headers: { Authorization: `Bearer ${tokens.token}` }
            });

            if (tokens.token && tokens.refreshToken) {
                locals.session.data = {
                    tokens,
                    expireIn: timeInDays,
                    user: userResponse.data
                };
            }
        } catch (err) {}
    }

    return {
        status: 302,
        headers: {
            location: `/?callback=true`
        }
    };
}
