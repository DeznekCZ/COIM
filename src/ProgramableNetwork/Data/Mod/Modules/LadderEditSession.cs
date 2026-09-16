using System;
using Mafi;
using ProgramableNetwork.Ladder;

namespace ProgramableNetwork;

// The working copy of one module's ladder program, shared by the two editor
// windows.
//
// The ladder view and the statement list are two renderings of the same text,
// so they cannot each own a buffer — the moment both are open, whichever one
// the player wasn't looking at would go stale.  Instead both bind here: the
// code window writes text in, the ladder window reads the parse out, and every
// mutation raises Changed so the other side redraws immediately.
//
// A single instance is registered globally, which also settles what "editing"
// means when a player opens an editor on a second module: BindTo swaps the
// session over and both windows follow. There is one edit in flight at a time,
// which is the behaviour the inspector already implies.
[GlobalDependency(RegistrationMode.AsSelf)]
public class LadderEditSession {

	private Module m_module;
	private string m_text = "";

	// Raised after any change to Module or Text. Windows subscribe on open and
	// unsubscribe on close, so a closed window never repaints.
	public event Action Changed;

	public Module Module => m_module;
	public string Text => m_text;

	// Last successful parse, or null when the text doesn't parse. Kept even
	// when Error is set by the tier check — a program that parses but needs a
	// bigger module should still DRAW, so the player can see what they wrote.
	public LadderProgram Program { get; private set; }

	// The DETACHED working copy the graphical editor edits.
	//
	// Diagram edits mutate this and derive text from it; they do not build text
	// and re-parse. That is what lets an operation be set up over several clicks
	// — arm a branch, pick its leg; mark a run, bypass it — because nothing
	// between those clicks throws the editor's state away. Null only while the
	// text doesn't parse, which is the one case where there is nothing coherent
	// to edit graphically.
	public LadderDocument Document { get; private set; }

	// Player-facing compile problem, or null when the program is clean for
	// this module's tier.
	public string Error { get; private set; }

	public LadderTier Tier => PlcLad.TierOf(m_module);

	public bool HasModule => m_module != null;

	// True while the working text still equals what the module compiled. The
	// ladder window gates its live power painting on this, because once they
	// diverge the machine's trace indices refer to a different program.
	public bool MatchesModule =>
		m_module != null && m_text == (m_module.Field["code", ""] ?? "");

	// Points the session at a module, reloading its saved program. Re-binding
	// to the SAME module is a no-op so that opening the second window doesn't
	// throw away edits the player has already made in the first.
	public void BindTo(Module module) {
		if (ReferenceEquals(m_module, module)) {
			return;
		}
		m_module = module;
		ReloadFromModule();
	}

	// Discards the working copy and pulls the module's saved program back in.
	// This is what Revert does.
	public void ReloadFromModule() {
		m_text = m_module?.Field["code", ""] ?? "";
		reparse();
		Changed?.Invoke();
	}

	public void SetText(string text) {
		string next = text ?? "";
		if (next == m_text) {
			// Early-out matters: it is what stops a window echoing its own
			// edit back into an endless Changed loop.
			return;
		}
		m_text = next;
		reparse();
		Changed?.Invoke();
	}

	// Runs a graphical edit against the working document.
	//
	// The document is the source of truth for the duration of the edit; text is
	// re-derived from it afterwards, not the other way round. `edit` returns
	// whether it changed anything, so a refused operation — a bracket the player
	// clicked, a move that would cross a branch — costs nothing and raises no
	// event, leaving selection and any pending placement exactly where they were.
	public bool Edit(Func<LadderDocument, bool> edit) {
		if (Document == null || edit == null) {
			return false;
		}
		if (!edit(Document)) {
			return false;
		}

		m_text = Document.ToText();
		revalidate();
		Changed?.Invoke();
		return true;
	}

	// True when the current text compiles for this module's tier — the gate
	// Save uses, so a player can't write a program the module cannot run.
	public bool CanCompile() {
		return Program != null && Error == null;
	}

	// Rewrites the working text in canonical form. No-op when the program
	// doesn't parse: there is nothing to canonicalise and the player would
	// lose a half-typed line.
	public void Tidy() {
		if (Program == null) {
			return;
		}
		SetText(Program.ToText());
	}

	private void reparse() {
		revalidate();
		// Text arrived from outside — the statement-list window, a revert, or a
		// module bind — so the working document is rebuilt from it. This is the
		// only direction text ever flows into the document; graphical edits go
		// the other way (see Edit).
		Document = Program != null ? new LadderDocument(Program) : null;
	}

	// Parse and tier-check are deliberately separate results: a parse failure
	// clears Program (nothing to draw), while a tier failure keeps it (draw it,
	// and tell the player which module to swap to).
	private void revalidate() {
		LadderProgram parsed = null;
		string error = null;

		try {
			parsed = LadderParser.Parse(m_text);
		} catch (Exception parseError) {
			error = parseError.Message;
		}

		if (parsed != null) {
			try {
				LadderValidator.Enforce(parsed, Tier);
			} catch (Exception tierError) {
				error = tierError.Message;
			}
		}

		Program = parsed;
		Error = error;
	}
}
