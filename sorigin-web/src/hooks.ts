import type User from '$lib/types/user'
import cookie from 'cookie'

export async function handle({ request, resolve }) {
    const cookies = cookie.parse(request.headers.cookie || '')

    if (cookies.token !== undefined) {
        request.locals.token = cookies.token as string
        request.locals.user = JSON.parse(cookies.user) as User
        request.locals.session_end = cookies.session_end as string
    }
    
    const response = await resolve(request)

    if (request.locals.logout !== undefined) {
        response.headers['set-cookie'] = getSettableCookies('logout', { }, 'Thu, 01 Jan 1970 00:00:00 GMT')
    }
    else if (request.locals.token !== undefined && cookies.token === undefined) {
        // If the cookies have not been set, set it!
        let fourHours = new Date()
        fourHours.setTime(fourHours.getTime() + 4 * 60 * 60 * 1000)
        response.headers['set-cookie'] = getSettableCookies(request.locals.token, request.locals.user, fourHours.toUTCString())
    }
    else if (request.locals.updated_user !== undefined && cookies.token !== undefined) {
        // If the user has been updated (username change, update the cookies.)
        response.headers['set-cookie'] = getSettableCookies(cookies.token, request.locals.updated_user, cookies.session_end)
    }

    return response
}

function getSettableCookies(token: string, user: any, expiresIn: string) {
    return [
        `session_end=pragma; expires=${expiresIn}; Path=/; HttpOnly`,
        `token=${token}; expires=${expiresIn}; Path=/; HttpOnly`,
        `user=${JSON.stringify(user)}; expires=${expiresIn}; Path=/; HttpOnly`,
    ]
}

export async function getSession(request) {
    return {
        user: request.locals.user,
        token: request.locals.token
    }
}