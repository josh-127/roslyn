﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings.MoveType;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.CodeActions.MoveType
{
    public partial class MoveTypeTests : CSharpMoveTypeTestsBase
    {
        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_SingleItem()
        {
            var code =
@"namespace N1
{
    class [||]Class1
    {
    }
}";

            var expected =
@"namespace N1
{
    class Class1
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtTop()
        {
            var code =
@"namespace N1
{
    class [||]Class1
    {
    }

    class Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    class Class1
    {
    }
}

namespace N1
{
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtTopWithComments()
        {
            var code =
@"namespace N1
{
    // Class1 Comment
    class [||]Class1
    {
    }

    // Class2 Comment
    class Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    // Class1 Comment
    class Class1
    {
    }
}

namespace N1
{
    // Class2 Comment
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtTopWithXmlComments()
        {
            var code =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class [||]Class1
    {
    }

    /// <summary>
    /// Class2 summary
    /// </summary>
    class Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class Class1
    {
    }
}

namespace N1
{
    /// <summary>
    /// Class2 summary
    /// </summary>
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtBottom()
        {
            var code =
@"namespace N1
{
    class Class1
    {
    }

    class [||]Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    class Class1
    {
    }
}

namespace N1
{
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtBottomWithComments()
        {
            var code =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }

    // Class2 comment
    class [||]Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }
}

namespace N1
{
    // Class2 comment
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemAtBottomWithXmlComments()
        {
            var code =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class Class1
    {
    }

    /// <summary>
    /// Class2 summary
    /// </summary>
    class [||]Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class Class1
    {
    }
}

namespace N1
{
    /// <summary>
    /// Class2 summary
    /// </summary>
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemInMiddle()
        {
            var code =
@"namespace N1
{
    class Class1
    {
    }

    class Class2
    {
    }

    class [||]Class3
    {
    }

    class Class4
    {
    }

    class Class5
    {
    }
}";

            var expected =
@"namespace N1
{
    class Class1
    {
    }

    class Class2
    {
    }
}

namespace N1
{
    class Class3
    {
    }
}

namespace N1
{
    class Class4
    {
    }

    class Class5
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemInMiddleWithComments()
        {
            var code =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }

    // Class2 comment
    class Class2
    {
    }

    // Class3 comment
    class [||]Class3
    {
    }

    // Class4 comment
    class Class4
    {
    }

    // Class5 comment
    class Class5
    {
    }
}";

            var expected =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }

    // Class2 comment
    class Class2
    {
    }
}

namespace N1
{
    // Class3 comment
    class Class3
    {
    }
}

namespace N1
{
    // Class4 comment
    class Class4
    {
    }

    // Class5 comment
    class Class5
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemInMiddleWithXmlComments()
        {
            var code =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class Class1
    {
    }

    /// <summary>
    /// Class2 summary
    /// </summary>
    class Class2
    {
    }

    /// <summary>
    /// Class3 summary
    /// </summary>
    class [||]Class3
    {
    }

    /// <summary>
    /// Class4 summary
    /// </summary>
    class Class4
    {
    }

    /// <summary>
    /// Class5 summary
    /// </summary>
    class Class5
    {
    }
}";

            var expected =
@"namespace N1
{
    /// <summary>
    /// Class1 summary
    /// </summary>
    class Class1
    {
    }

    /// <summary>
    /// Class2 summary
    /// </summary>
    class Class2
    {
    }
}

namespace N1
{
    /// <summary>
    /// Class3 summary
    /// </summary>
    class Class3
    {
    }
}

namespace N1
{
    /// <summary>
    /// Class4 summary
    /// </summary>
    class Class4
    {
    }

    /// <summary>
    /// Class5 summary
    /// </summary>
    class Class5
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemInMiddleWithInterface()
        {
            var code =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }

    // IClass3 comment
    interface IClass3
    {
        void DoStuff();
    }

    // Class3 comment
    class [||]Class3 : IClass3
    {
        public void DoStuff() { }
    }

    // Class4 comment
    class Class4
    {
    }

    // Class5 comment
    class Class5
    {
    }
}";

            var expected =
@"namespace N1
{
    // Class1 comment
    class Class1
    {
    }

    // IClass3 comment
    interface IClass3
    {
        void DoStuff();
    }
}

namespace N1
{
    // Class3 comment
    class Class3 : IClass3
    {
        public void DoStuff() { }
    }
}

namespace N1
{
    // Class4 comment
    class Class4
    {
    }

    // Class5 comment
    class Class5
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_TwoItemsInDifferentNamespace()
        {
            var code =
@"namespace N1
{
    class [||]Class1
    {
    }
}

namespace N2
{
    class Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    class Class1
    {
    }
}

namespace N2
{
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsMoveType)]
        public Task MoveType_NamespaceScope_ItemsInDifferentNamespace()
        {
            var code =
@"namespace N1
{
    interface IClass1
    {
    }

    class [||]Class1 : IClass1
    {
    }
}

namespace N2
{
    class Class2
    {
    }
}";

            var expected =
@"namespace N1
{
    interface IClass1
    {
    }
}

namespace N1
{
    class Class1 : IClass1
    {
    }
}

namespace N2
{
    class Class2
    {
    }
}";

            return TestNamespaceMove(code, expected, expectOperation: false);
        }

        private async Task TestNamespaceMove(string originalCode, string expectedCode, bool expectOperation = true)
        {
            using (var workspace = CreateWorkspaceFromOptions(originalCode, default))
            {
                var documentToModifyId = workspace.Documents[0].Id;
                var textSpan = workspace.Documents[0].SelectedSpans[0];
                var documentToModify = workspace.CurrentSolution.GetDocument(documentToModifyId);

                var moveTypeService = documentToModify.GetLanguageService<IMoveTypeService>();
                Assert.NotNull(moveTypeService);

                var refactorings = await moveTypeService.GetRefactoringAsync(documentToModify, textSpan, MoveTypeOperationKind.MoveTypeScope, CancellationToken.None).ConfigureAwait(false);
                Assert.NotEmpty(refactorings);

                foreach (var refactoring in refactorings)
                {
                    Assert.True(refactoring.IsApplicable(workspace));
                    var operations = await refactoring.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);

                    if (expectOperation)
                    {
                        Assert.NotEmpty(operations);
                    }

                    foreach (var operation in operations)
                    {
                        operation.Apply(workspace, CancellationToken.None);
                    }
                }

                var modifiedDocument = workspace.CurrentSolution.GetDocument(documentToModifyId);
                var formattedDocument = await Formatter.FormatAsync(modifiedDocument).ConfigureAwait(false);

                var formattedText = await formattedDocument.GetTextAsync().ConfigureAwait(false);
                Assert.Equal(expectedCode, formattedText.ToString());
            }
        }
    }
}
