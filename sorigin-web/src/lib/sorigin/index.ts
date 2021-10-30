import { browser } from '$app/env';
import type User from '$lib/types/user';
import { REFETCH_URL, SORIGIN_URL } from '$lib/utils/env';

export interface UserResponse {
    user: User | null;
    error: string | null;
}

export function soriginURL() {
    return browser ? SORIGIN_URL : REFETCH_URL;
}

export async function userByUsername(name: string) {
    const backend = browser ? SORIGIN_URL : REFETCH_URL;
    const url = `${backend}/api/user/by-username/${name.toLowerCase()}`;
    const res = await fetch(url);

    return await genUserResponse(res);
}

export async function updateDescription(newBio: string) {
    const bioUpdateURL = `${browser ? SORIGIN_URL : REFETCH_URL}/api/user/edit/description`;

    const res = await fetch(bioUpdateURL, {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({
            newDescription: newBio
        })
    });

    return await genUserResponse(res);
}

export async function getUsernames() {
    const usernameListURL = `${browser ? SORIGIN_URL : REFETCH_URL}/api/user`;
    const userRes = await fetch(usernameListURL, {
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        }
    });

    if (userRes.ok) {
        const usersBody = await userRes.json();
        const users = usersBody as string[];
        return users;
    }
    return [] as string[];
}

async function genUserResponse(res: Response) {
    if (res.ok) {
        const user: User = await res.json();
        return { user: user, error: null } as UserResponse;
    } else {
        try {
            const body = await res.json();
            if (body.error !== undefined) {
                return { user: null, error: body.error } as UserResponse;
            }
        } finally {
            return { user: null, error: res.statusText };
        }
    }
}
