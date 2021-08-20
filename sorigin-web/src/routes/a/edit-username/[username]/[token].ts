import { REFETCH_URL } from '../../../../utils/env'
import type User from '$lib/types/user'
import fetch from 'node-fetch'

const editUsernameURL = `${REFETCH_URL}/api/user/edit/username`

async function editUsername(newName: string, token: string): Promise<User | null> {
    const res = await fetch(editUsernameURL, {
        method: 'POST',
        headers: {
            Accept: 'application/json',
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ 
            username: newName
        })
    })
    if (!res.ok)
        return null
    const body = await res.json()
    return body as User
}

export async function get({ params, locals }) {
    const username = params.username
    const token = params.token
    const user = await editUsername(username, token)
    if (user === null) {
        return {
            status: 403
        }
    }
    locals.updated_user = user
    return {
        status: 302,
        headers: {
            location: `/@${user.username}`
        }
    }
}