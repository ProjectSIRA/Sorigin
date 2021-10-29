import { REFETCH_URL } from '$lib/utils/env'

const revokeTokenURL = `${REFETCH_URL}/api/auth/refresh`

export async function get({ query, locals }) {
    
    const refreshToken = query.get('refreshToken')
    if (refreshToken !== null && refreshToken !== undefined) {
        await fetch(`${revokeTokenURL}?refreshToken=${query.get('refreshToken')}`, {
            method: 'POST'
        })
    }

    locals.logout = { }
    return {
        status: 302,
        headers: {
            location: '/'
        }
    }
}