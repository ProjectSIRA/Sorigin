import type User from '$lib/types/user'
import { REFETCH_URL } from '$lib/utils/env'
import fetch from 'node-fetch'

const tokenURL = `${REFETCH_URL}/api/auth/login`
const userURL = `${REFETCH_URL}/api/auth/@me`

async function getAccessToken(grant: string): Promise<string | null> {
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
    return body.token as string
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

export async function get(req) {
    const code = req.query.get('grant')
    const accessToken = await getAccessToken(code)
    if (accessToken === null) {
        return {
            status: 403
        }
    }

    const user = await getUser(accessToken)
    if (user === null) {
        return {
            status: 403
        }
    }

    req.locals.user = user
    req.locals.token = accessToken

    return {
        status: 302,
        headers: {
            location: '/'
        }
    }
}