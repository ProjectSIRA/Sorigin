export async function get(req) {
    req.locals.logout = { }
    return {
        status: 302,
        headers: {
            location: '/'
        }
    }
}