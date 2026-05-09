import { markVoted, hasVoted } from '../utils/voteStorage';

describe('voteStorage', () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it('markVoted_and_hasVoted_returnsTrueImmediately', () => {
        markVoted('test-slug');
        expect(hasVoted('test-slug')).toBe(true);
    });

    it('hasVoted_expiredToken_returnsFalse', () => {
        const expiredTs = Date.now() - 366 * 86_400_000;
        localStorage.setItem('voted_test-slug', JSON.stringify({ ts: expiredTs }));
        expect(hasVoted('test-slug')).toBe(false);
    });

    it('hasVoted_noEntry_returnsFalse', () => {
        expect(hasVoted('unknown-slug')).toBe(false);
    });
});
