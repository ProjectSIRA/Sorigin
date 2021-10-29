import type TokenSet from '$lib/types/tokenSet'
import type User from '$lib/types/user'
import { REFETCH_URL } from '$lib/utils/env'
import fetch from 'node-fetch'

const tokenURL = `${REFETCH_URL}/api/auth/login`
const userURL = `${REFETCH_URL}/api/auth/@me`

async function getTokens(grant: string): Promise<TokenSet | null> {
    const res = await fetch(`${tokenURL}?grant=${grant}&platform=discord`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json'
        }
    })
    if (!res.ok)
        return null
    const body = await res.json()
    return body as TokenSet
}

async function getUser(accessToken: string): Promise<User | null> {
    const res = await fetch(userURL, {
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${accessToken}`
        }
    })
    if (!res.ok)
        return null
    const body = await res.json()
    return body as User
}

export async function get(req: any) {
    const code = req.query.get('grant')
    const tokens = await getTokens(code)
    if (tokens === null) {
        return {
            status: 403
        }
    }

    const user = await getUser(tokens.token)
    if (user === null) {
        return {
            status: 403
        }
    }

    req.locals.user = user
    req.locals.token = tokens.token
    req.locals.refresh = tokens.refreshToken

    return {
        status: 302,
        headers: {
            location: '/'
        }
    }
}